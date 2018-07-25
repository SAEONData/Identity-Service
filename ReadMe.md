# Identity Service #
The South African Environmental Observation Network (SAEON) Identity Service
provides single sign-on for all SAEON web applications.

For more information see the [SAEON website](http://www.saeon.ac.za)

To install on server with IIS

* Copy saeon.ac.za.pfx into Config directory
* Allow 'Network Service' read access to Config directory
* Set Application Pool Identity to 'Network Service'

