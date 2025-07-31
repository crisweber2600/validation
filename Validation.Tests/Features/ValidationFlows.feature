Feature: Validation flow registration
    Scenario: Register flows from config
        Given a JSON configuration for Item flow
        When services are configured with AddValidationFlows
        Then SaveValidationConsumer for Item can be resolved
        And SaveCommitConsumer for Item can be resolved
