# validation

## Validation Flow Configuration

`AddValidationFlows` can be configured via JSON. Each entry describes which consumers to register and any validation plan settings.

```json
[
  {
    "Type": "Namespace.Type, Assembly",
    "SaveValidation": true,
    "SaveCommit": true,
    "DeleteValidation": true,
    "DeleteCommit": true,
    "MetricProperty": "Metric",
    "ThresholdType": 1,
    "ThresholdValue": 0.2,
    "ManualRules": [
      { "Property": "Metric", "MinValue": 0 }
    ]
  }
]
```

Boolean flags control which message consumers are added. `ManualRules` allows simple manual validation rules where `Property` refers to a numeric property and `MinValue` specifies the minimum allowed value.

## Samples

Example configuration files can be found under the `config` folder.

