//users
//-> by_email
function (doc) {
    if (doc.type == "user") {
        emit(doc.email, null);
    }
}

//-> by_username
function (doc) {
    if (doc.type == "user") {
        emit(doc.username, null);
    }
}

function (doc) {
    if (doc.geo.loc) {
        emit(
            {
                type: "Point",
                coordinates: [parseFloat(doc.geo.loc[0]), parseFloat(doc.geo.loc[1])]
            },
            [doc._id, doc.geo.loc]);
    }
};

//beers
//-> by_name
function (doc) {
    if (doc.type == "beer") {
        emit(doc.name, doc._id);
    }
}

//-> by_brewery
function (doc) {
    if (doc.type == "beer" && doc.brewery) {
        emit(doc.brewery, null);
    }
}

//breweries
//-> by_name
function (doc) {
    if (doc.type == "brewery") {
        emit(doc.name, null);
    }
}

//taps
//-> by_location
function (doc) {
    if (doc.type == "tap" && doc.place) {
        emit(
            {
                type: "Point",
                coordinates: [doc.place.lat, doc.place.long]
            },
            [doc._id, doc.place]);
    }
};