﻿TapMap
=======

This app demonstrates using ASP.NET MCV (3) with the .NET Client Library for [Couchbase Server](http://couchbase.com).

## Setup:

* Couchbase >= Server 2.0 DP4 
* Create a bucket named "beernique", password "b33rs"
* Open your the TapMap web site and browse to http://localhost/TapMap/Setup and submit the form.
** The default SetupKey in AppConfig is "8675309" without the quotes.
** Setup creates 4 users, john, paul, ringo, george with passwords vocals, bass, drums, guitar respectively.
** Setup creates sample taps.  Select "Greater Boston" from the home page and view the taps on the map.

## Configuration:

* By default, it is assumed that your site is going to work with a Couchbase Server node on localhost.	
* IIS or IIS Express is required.  There is some basic security code that won't work with the development server.

## Usage

* Browse to your site and create a user account.
* Create a tap.  
* Browse the taps on the map by selecting a bounding box (region) from the home page.
* If you're not in the Northeast or Northwest, you'll need to create a region to query by.
