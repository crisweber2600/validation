Feature: Validation flow registration
  Scenario: Services are registered from config
    Given a valid item flow configuration
    When services are built
    Then SaveValidationConsumer can be resolved for Item
    And SaveCommitConsumer can be resolved for Item
