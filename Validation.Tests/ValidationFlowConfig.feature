Feature: Validation flow configuration
  Scenario: Config loads and services resolved
    Given a validation flow configuration
    When I load the options and configure services
    Then services for Item are resolvable
