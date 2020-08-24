Feature: Map
	In order to report a location I need to display a map

Scenario: Map standard use
	Given I navigate to "/ui-map/page-one"
	Then I wait one second
	Then I wait one second
	Then I should see the header
	Then I should find an element with class ".leaflet-map"