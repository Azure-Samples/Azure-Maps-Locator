// Azure Maps Store Locator (version 1.0-rc.1)
// Copyright (c) Microsoft Corporation. All rights reserved.
// https://github.com/Azure-Samples/Azure-Maps-Locator
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

class storelocator {

    map = null;                            // Azure Maps control
    options = null;                        // Options for the store locator

    // Search related properties.
    search = {
        url: 'https://{azMapsDomain}/search/fuzzy/json?api-version=1.0&query={query}&typeahead={typeahead}&limit={limit}&countrySet={countrySet}&language={language}&lat={lat}&lon={lon}&view={view}',
        control: null,                     // search input div
        input: {
            minLength: 3,
            currentLength: 0,
            keyStrokeDelay: 250
        },
        autocomplete: {
            control: null,                 // autocomplete div
            locateMeTemplate: '',          // locate me template
            locationsTemplate: '',         // locations template
            storesTemplate: '',            // stores template
        },
        resultsControl: null,              // results div
        resultsTemplate: '',               // results template
        datasource: new atlas.source.DataSource(),
        userLocation: [0, 0]                // user location marker
    };

    // Isochrone related properties.
    isochrone = {
        url: 'https://{azMapsDomain}/route/range/json?api-version=1.0&query={query}&traffic={traffic}&travelMode={travelMode}&timeBudgetInSec={timeBudgetInSec}',
        walkingDataSource: new atlas.source.DataSource(),
        drivingDataSource: new atlas.source.DataSource()
    };

    // Store related properties.
    store = {
        datasource: new atlas.source.DataSource(),
        popup: new atlas.Popup({
            position: [0, 0],
            pixelOffset: [0, -18]
        }),
        template: ''                       // store popup template
    };


    /**
    * Constructor for the Store Locator.
    * @param {object} map - The map instance to be used for rendering.
    * @param {object} options - Options to customize the Store Locator.
    */
    constructor(map, options = {}) {
        this.map = map;                    // Set the map instance for Azure Maps.
        this.options = {
            countrySet: ['US'],            // An array of country region ISO2 values to limit searches to
            language: 'en-US',             // Language in which search results should be returned.
            rangeInKm: 25,                 // Default range in km for search
            maxSearchResults: 5,           // Default max search results to show in list. (max 9)
            walkingTime: 15,               // Default max walking time in minutes
            drivingTime: 15,               // Default max driving time in minutes
            ...options                     // Merge provided options with default values.
        };

        this.initializeTemplates();
        this.initializeSearch();
        this.initializeIsochroneLayers();
        this.initializeStoresLayer();
        this.initializeFilters();
    }


    /**
     * Initializes the templates used by the Store Locator.
     */
    initializeTemplates() {
        // Get the store popup template from the main page.
        this.store.template = document.getElementById('template-storepopup').innerHTML;

        // Get the search autocomplete template's from the main page.
        this.search.autocomplete.locateMeTemplate = document.getElementById('template-autocomplete-locateMe').innerHTML;
        this.search.autocomplete.locationsTemplate = document.getElementById('template-autocomplete-locations').innerHTML;
        this.search.autocomplete.storesTemplate = document.getElementById('template-autocomplete-stores').innerHTML;

        // Get the search results template from the main page.
        this.search.resultsTemplate = document.getElementById('template-results').innerHTML;
    }

    initializeFilters() {

        this.getFeatures().then((categories) => {
            const control = document.getElementById('locator-filters');
            const categoryTemplate = document.getElementById('template-filters-category').innerHTML;
            const tagTemplate = document.getElementById('template-filters-tag').innerHTML;

            var html = '';
            for (const category of categories) {

                var innerHtml = '';
                for (const tag of category.tags) {
                    innerHtml += tagTemplate
                        .replaceAll('{id}', tag.id)
                        .replace('{name}', tag.name);
                }

                html += categoryTemplate
                    .replaceAll('{id}', category.id)
                    .replace('{name}', category.name)
                    .replace('{expanded}', html == '' ? 'true' : 'false')
                    .replace('{show}', html == '' ? 'show' : '')
                    .replace('{tags}', innerHtml);
            }

            control.innerHTML += html;
        });
    }

    /**
     * Initializes the search control and related event listeners.
     */
    initializeSearch() {
        this.search.autocomplete.control = document.getElementById('locator-autocomplete');
        this.search.resultsControl = document.getElementById('locator-results');
        this.search.control = document.getElementById('locator-search');

        this.search.control.addEventListener('keyup', (e) => { this.searchControlKeyupEvent(e) });
        this.search.control.addEventListener('search', (e) => { this.searchControlSearchEvent(e) });
        this.search.control.addEventListener('focus', (e) => {
            const trimmedValue = this.search.control.value.trim();
            if (trimmedValue.length == 0) {
                this.search.autocomplete.control.className = 'dropdown-menu show';
                this.search.autocomplete.control.innerHTML = this.search.autocomplete.locateMeTemplate;
            }
        });
    }


    /**
     * Initializes the stores data sources and layer for store pins.
     */
    initializeStoresLayer() {
        // Create a layer that defines how to render the stores on the map.
        const storesLayer = new atlas.layer.SymbolLayer(this.store.datasource, 'stores', {
            textOptions: {
                textField: ['get', 'orderNumber'], // Specify the property name that contains the text you want to appear with the symbol.
                offset: [0, -1.5],
                color: '#ffffff',
                font: ['SegoeUi-Bold']
            },
            iconOptions: {
                image: 'marker-red'
            }
        });
        this.map.sources.add(this.store.datasource);
        this.map.layers.add(storesLayer);

        // Add a click event to the stores layer.
        this.map.events.add('click', storesLayer, (e) => {
            // Make sure the event occurred on a shape feature.
            if (e.shapes && e.shapes.length > 0) {
                this.showPopup(e.shapes[0]);
            }
        });
    }

    /**
     * Initializes the isochrone data sources and layers for walking and driving polygons.
     */
    initializeIsochroneLayers() {
        // Add a layer for the walking isochrone.
        const walkingLineLayer = new atlas.layer.LineLayer(this.isochrone.walkingDataSource, 'walking', {
            strokeColor: 'Green',
            strokeWidth: 1,
            strokeDashArray: [3, 3]
        });
        this.map.sources.add(this.isochrone.walkingDataSource);
        this.map.layers.add(walkingLineLayer);

        // Add a layer for the driving isochrone.
        const drivingLineLayer = new atlas.layer.LineLayer(this.isochrone.drivingDataSource, 'driving', {
            strokeColor: 'Blue',
            strokeWidth: 1,
            strokeDashArray: [3, 3]
        });
        this.map.sources.add(this.isochrone.drivingDataSource);
        this.map.layers.add(drivingLineLayer);
    }


    /**
     * Opens the popup and shows the details of the store.
     * @param {object} shape - The shape that was clicked on.
     */
    showPopup(shape) {
        this.store.popup.close();

        var store = shape.getProperties();
        var position = shape.getCoordinates();

        this.store.popup.setOptions({
            // Update the content of the popup.
            content: this.store.template
                .replace('{id}', store.id)
                .replace('{imageUrl}', store.imageUrl)
                .replace('{webUrl}', store.webUrl)
                .replace('{title}', store.name)
                .replace('{name}', store.name)
                .replace('{address}', store.address.streetAddressLine1)
                .replace('{city}', store.address.city)
                .replace('{country}', store.address.countryName)
                .replace('{distanceInKm}', store.distanceInKm.toFixed(1))
                .replace('{distanceInMile}', this.convertToMiles(store.distanceInKm).toFixed(1))
                .replace('{directionsUrl}', `https://www.bing.com/maps?rtp=pos.${this.search.userLocation[1]}_${this.search.userLocation[0]}_My%20current%20location~pos.${store.location.coordinates[1]}_${store.location.coordinates[0]}_${store.name}`),
            // Update the position of the popup with the pins coordinate.
            position: position
        });

        // Open the popup.
        this.store.popup.open(this.map);
    }


    /**
     * Handels the store ckick event, centers the map and opens the store popup.
     * @param {string} id
     */
    storeClicked(id) {
        var shape = this.store.datasource.getShapeById(id);
        var position = shape.getCoordinates();

        // Center the map over the clicked item from the result list.
        this.map.setCamera({
            center: [position[0], position[1] - 0.001], // we remove a small offset to center the popup on the map.
            zoom: 16
        });

        this.showPopup(shape);
    }


    /**
     * Handles the search event for the search control, hiding the autocomplete list if the input is too short.
     * @param {Event} e - The search event object.
     */
    searchControlSearchEvent(e) {
        const trimmedValue = this.search.control.value.trim();
        const minLength = this.search.input.minLength;

        if (trimmedValue.length < minLength) {
            // Hide the autocomplete list.
            this.search.autocomplete.control.className = 'dropdown-menu';
            this.search.autocomplete.control.innerHTML = '';
        }
    }

    locateMe() {
        // Check to see if the user has allowed the browser to use their location.
        navigator.geolocation.getCurrentPosition((position) => {

            // update the search input with the selected search from the autocompleet.
            this.search.control.value = '';

            // Show the nearby stores.
            this.showNearbyStores([position.coords.longitude, position.coords.latitude]);
        }, (error) => {
            // If an error occurs when trying to access the users position, display an error message.
            alert('Sorry, your position information is unavailable or not accasable!');
        },
            { enableHighAccuracy: true }
        );
    }


    /**
    * Handles the keyup event for the search control, triggering fuzzy search and handling Enter key press.
    * @param {Event} e - The keyup event object.
    */
    searchControlKeyupEvent(e) {
        const trimmedValue = this.search.control.value.trim();
        const minLength = this.search.input.minLength;
        const currentLength = trimmedValue.length;

        if (currentLength >= minLength) {
            setTimeout(() => {
                // Only perform the search if the input value hasn't changed.
                if (this.search.input.currentLength === currentLength) {
                    this.fuzzySearch(trimmedValue);
                }
            }, this.search.input.keyStrokeDelay);

            // Handle Enter key press.
            if (e.keyCode === 13) {
                var shapes = this.search.datasource.getShapes();
                if (shapes && shapes.length > 0) {
                    this.searchClicked(shapes[0].data.id);
                }
            }
        } else {
            // Hide the autocomplete list.
            this.search.autocomplete.control.className = 'dropdown-menu';
            this.search.autocomplete.control.innerHTML = '';
        }

        this.search.input.currentLength = currentLength;
    }

    getCategoryIcon = (categoryId) => {
        const firstFourDigits = categoryId > 9999 ? Math.floor(categoryId / 1000) : categoryId;

        switch (firstFourDigits) {
            case 7380: // railroad station
            case 9942: // public transportation stop
                return 'bi-train-front';
            case 7383: // airport
                return 'bi-airplane';
            case 7315: // restaurant
            case 9376: // café/pub
            case 9361: // shop
            case 9359: // restaurant area
                return 'bi-shop';
            case 9663: // health care service
            case 7321: // hospital
            case 9373: // doctor
            case 9374: // dentist
            case 7326: // pharmacy
            case 9956: // emergency room
            case 7391: // emergency medical service
                return 'bi-hospital';
            case 7324: // post office
                return 'bi-envelope';
            case 7377: // college/university
            case 7372: // school
            case 9913: // library
                return 'bi-mortarboard';
            case 9352: // company
            case 7327: // department store
            case 7373: // shopping center
            case 7328: // bank
                return 'bi-building';
            case 7311: // gas station
                return 'bi-fuel-pump';
            case 7309: // electric vehicle charging station
                return 'bi-ev-station';
            default:
                return 'bi-geo-alt'; // default icon for address
        }
    };


    /**
    * Performs a fuzzy search using the given query to find locations and points of interest.
    * @param {string} query - The search query to find locations and points of interest.
    */
    fuzzySearch(query) {
        const searchRequestUrl = this.search.url.replace('{query}', encodeURIComponent(query))
            .replace('{typeahead}', true)
            .replace('{countrySet}', this.options.countrySet)
            .replace('{language}', this.options.language)
            .replace('{lon}', this.map.getCamera().center[0])
            .replace('{lat}', this.map.getCamera().center[1])
            .replace('{limit}', 4)
            .replace('{view}', 'Auto');

        Promise.all([
            this.processRequest(searchRequestUrl),
            this.searchStores(query, 4)
        ]).then(response => {
            let html = '';
            let points = [];

            html += '<li><h6 class="dropdown-header">Locations</h6></li>';

            for (const item of response[0].results) {
                const isPOI = item.poi && item.poi.name; // check if the result is a point of interest

                const icon = isPOI ? this.getCategoryIcon(item.poi.categorySet[0].id) : 'bi-geo-alt';
                const name = isPOI ? item.poi.name : item.address.freeformAddress;
                const description = isPOI ? item.address.freeformAddress : '';

                html += this.search.autocomplete.locationsTemplate
                    .replace('{id}', item.id)
                    .replace('{name}', name)
                    .replace('{description}', description)
                    .replace('{icon}', icon)
                    .replace('{longitude}', item.position.lon)
                    .replace('{latitude}', item.position.lat);

                const position = new atlas.data.Position(item.position.lon, item.position.lat);
                const point = new atlas.data.Feature(new atlas.data.Point(position), item, item.id);
                points.push(point);
            }

            if (response[0].results.length == 0)
                html += '<li><a class="dropdown-item disabled" aria-disabled="true">No Results Found</a></li>';

            this.search.datasource.clear();
            this.search.datasource.add(points);

            html += '<li><hr class="dropdown-divider"></li>';
            html += '<li><h6 class="dropdown-header">Stores</h6></li>';

            for (const store of response[1]) {
                html += this.search.autocomplete.storesTemplate
                    .replace('{id}', store.id)
                    .replace('{imageUrl}', store.imageUrl)
                    .replace('{webUrl}', store.webUrl)
                    .replace('{title}', store.name)
                    .replace('{name}', store.name)
                    .replace('{address}', store.address.streetAddressLine1)
                    .replace('{city}', store.address.city)
                    .replace('{country}', store.address.countryName);
            }

            if (response[1].length == 0)
                html += '<li><a class="dropdown-item disabled" aria-disabled="true">No Results Found</a></li>';

            // Update the UI.
            this.search.autocomplete.control.innerHTML = html;
            this.search.autocomplete.control.className = 'dropdown-menu show';
        });
    }

    /**
    * Handle the search event when a store location is clicked on the autocompleet list.
    */
    searchClicked(id) {
        const shape = this.search.datasource.getShapeById(id);
        const coordinates = shape.getCoordinates();
        const item = shape.getProperties();

        // update the search input with the selected search from the autocompleet.
        this.search.control.value = (item.poi && item.poi.name) ? item.poi.name : item.address.freeformAddress;

        // Show the nearby stores.
        this.showNearbyStores(coordinates);
    }

    showStoresByCountry(country) {
        // Get stores bt Country.
        this.getStoresByCountry(country).then((stores) => {

            let points = []; // stores the point features for the stores.

            stores.forEach((store) => {

                store.distanceInKm = 0;

                // Create a position object from the lon and lat values, and
                // Create a Point feature and pass in the store object as the properties so that we can access them later if needed.
                const position = new atlas.data.Position(store.location.coordinates[0], store.location.coordinates[1]);
                const point = new atlas.data.Feature(new atlas.data.Point(position), store, store.id);

                points.push(point);
            });

            // Clear the data source and add stores point data to the data source.
            this.store.datasource.clear();
            this.store.datasource.add(points);

            this.map.resize();
        });
    }

    showNearbyStores(coordinates) {

        // store the location as starting point.
        this.search.userLocation = coordinates;

        // Hide the autocomplete list.
        this.search.autocomplete.control.className = 'dropdown-menu';

        // Close any previously open popup.
        this.store.popup.close();

        // Add a pin to the map to the users search location.
        this.map.markers.clear();
        this.map.markers.add(new atlas.HtmlMarker({
            color: 'DodgerBlue',
            htmlContent: '<i class="bi bi-dot" style="font-size: 2rem; color: {color};"></i>',
            position: coordinates,
            anchor: 'center'
        }));

        // Center the map over the result.
        this.map.setCamera({
            center: coordinates,
            zoom: 10
        });

        // Get nearby stores.
        this.getNearbyStores(coordinates).then((stores) => {

            let html = '';   // used to store the HTML for the list of stores.
            let count = 0;   // used to limit the number of stores shown in the list.
            let points = []; // stores the point features for the stores.

            // Sort stores by distance.
            stores.sort((a, b) => a['distanceInKm'] - b['distanceInKm']);

            stores.forEach((store) => {

                store.orderNumber = count + 1;

                // Create a position object from the lon and lat values, and
                // Create a Point feature and pass in the store object as the properties so that we can access them later if needed.
                const position = new atlas.data.Position(store.location.coordinates[0], store.location.coordinates[1]);
                const point = new atlas.data.Feature(new atlas.data.Point(position), store, store.id);
                points.push(point);

                // Create a list item for each store.
                if (count++ < this.options.maxSearchResults) {
                    html += this.search.resultsTemplate
                        .replace('{id}', store.id)
                        .replace('{imageUrl}', store.imageUrl)
                        .replace('{name}', store.name)
                        .replace('{address}', store.address.streetAddressLine1)
                        .replace('{city}', store.address.city)
                        .replace('{country}', store.address.countryName)
                        .replace('{distanceInKm}', store.distanceInKm.toFixed(1))
                        .replace('{distanceInMile}', this.convertToMiles(store.distanceInKm).toFixed(1))
                        .replace('{orderNumber}', store.orderNumber)
                        .replace('{longitude}', position[0])
                        .replace('{latitude}', position[1]);
                }
            });

            // Clear the data source and add stores point data to the data source.
            this.store.datasource.clear();
            this.store.datasource.add(points);

            // Add stores to the list in the UI.
            this.search.resultsControl.innerHTML = html;

            // If no stores are found, display a message to the user.
            if (stores.length == 0) {
                this.search.resultsControl.innerHTML = '<div class="alert alert-warning alert-dismissible" role="alert">No Results Found.<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>';
            }

            this.map.resize();
        });

        // Draw isochrones.
        this.drawDrivingIsochrone(coordinates);
        this.drawWalkingIsochrone(coordinates);
    }


    /**
    * Draw a driving isochrone on the map based on the provided position (latitude and longitude) and driving time budget.
    * @param {number[]} position - An array containing the latitude (index 0) and longitude (index 1) of the location.
    * @returns {void} This function does not have a direct return value. It draws the driving isochrone on the map.
    */
    async drawDrivingIsochrone(position) {
        const requestUrl = this.isochrone.url
            .replace('{query}', [position[1], position[0]])
            .replace('{traffic}', true)
            .replace('{travelMode}', 'car')
            .replace('{timeBudgetInSec}', this.options.drivingTime * 60);

        try {
            const response = await this.processRequest(requestUrl);
            const coordinates = new atlas.data.Position.fromLatLngs(response.reachableRange.boundary);

            // Retrieve the first coordinate from the array
            const firstCoordinate = coordinates[0];

            // Add the first coordinate to the end of the array
            coordinates.push(firstCoordinate);

            this.isochrone.drivingDataSource.clear();
            this.isochrone.drivingDataSource.add(new atlas.data.LineString(coordinates));
        } catch (error) {
            // Handle the error appropriately, e.g., display a message to the user.
            console.error('Error drawing driving isochrone:', error);
        }
    }


    /**
    * Draw a walking isochrone on the map based on the provided position (latitude and longitude) and walking time budget.
    * @param {number[]} position - An array containing the latitude (index 0) and longitude (index 1) of the location.
    * @returns {void} This function does not have a direct return value. It draws the walking isochrone on the map.
    */
    async drawWalkingIsochrone(position) {
        const requestUrl = this.isochrone.url
            .replace('{query}', [position[1], position[0]])
            .replace('{traffic}', false)
            .replace('{travelMode}', 'pedestrian')
            .replace('{timeBudgetInSec}', this.options.walkingTime * 60);

        try {
            const response = await this.processRequest(requestUrl);
            const coordinates = new atlas.data.Position.fromLatLngs(response.reachableRange.boundary);

            // Retrieve the first coordinate from the array
            const firstCoordinate = coordinates[0];

            // Add the first coordinate to the end of the array
            coordinates.push(firstCoordinate);

            this.isochrone.walkingDataSource.clear();
            this.isochrone.walkingDataSource.add(new atlas.data.LineString(coordinates));
        } catch (error) {
            // Handle the error appropriately, e.g., display a message to the user.
            console.error('Error drawing walking isochrone:', error);
        }
    }

    async getStoresByCountry(country) {
        // Get the stores within the specified country.
        const response = await fetch(`/api/stores/search?country=${country}`);

        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.status} ${response.statusText}`);
        }

        return await response.json();
    }

    async getFeatures() {
        const response = await fetch('/api/stores/features');

        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.status} ${response.statusText}`);
        }

        return await response.json();
    }


    /**
     * Fetch nearby stores from the API based on the given position (latitude and longitude) and range in kilometers.
     * @param {number[]} position - An array containing the latitude (index 0) and longitude (index 1) of the location.
     * @returns {Promise} A Promise that resolves to the parsed JSON data from the API response.
     * @throws {Error} Throws an error if the network response is not successful.
     */
    async getNearbyStores(position) {
        // Get the stores within the specified range.
        const response = await fetch(`/api/stores/search?latitude=${position[1]}&longitude=${position[0]}&rangeInKm=${this.options.rangeInKm}`);

        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.status} ${response.statusText}`);
        }

        return await response.json();
    }

    /**
     * Search stores by query.
     */
    async searchStores(query, limit = 5) {

        query = encodeURIComponent(query);

        const response = await fetch(`/api/stores/search?query=${query}&limit=${limit}`);

        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.status} ${response.statusText}`);
        }

        return await response.json();
    }

    /**
    * This is a reusable function that sets the Azure Maps platform domain,
    * signs the request, and makes use of any transformRequest set on the map.
    * @param {string} url - The URL for the API request.
    * @returns {Promise} A Promise that resolves to the parsed JSON data from the API response.
    * @throws {Error} Throws an error if the network response is not successful.
    */
    async processRequest(url) {
        // Replace the domain placeholder to ensure the same Azure Maps is used throughout the app.
        url = url.replace('{azMapsDomain}', atlas.getDomain());

        // Get the authentication details from the map for use in the request.
        var requestParams = this.map.authentication.signRequest({ url: url });

        // Transform the request.
        var transform = this.map.getServiceOptions().tranformRequest;
        if (transform) requestParams = transform(url);

        const response = await fetch(requestParams.url, {
            method: 'GET',
            mode: 'cors',
            headers: new Headers(requestParams.headers)
        });

        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.status} ${response.statusText}`);
        }

        return await response.json();
    }

    /**
     * Convert kilometers to miles.
     * @param {number} km - The distance in kilometers.
     * @returns {number} The distance in miles.
     */
    convertToMiles(km) {
        return km > 0 ? km / 1.60934 : 0;
    }
}