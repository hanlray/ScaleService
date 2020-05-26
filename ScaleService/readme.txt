FontSize:
0 12x12
1 16x16
2 24x24

ShowStyle:
1 从右向左
2 从左向右
3 从下向上

ShowSpeed:
1-8 数值越大，移动越慢

"Bidirectionals": [
    {
      "Gratings": [8,7],
      "Entrance": {
        "Name": "进口",
        "Button": 6,
        "Gratings": {
          "Front": 8,
          "Back": 7
        },
        "Timeout": 5000,
        "ScaleIP": "192.168.1.111",
        "InOrOut": 1,
        "LED": {
          "IP": "192.168.1.222",
          "MaterialUid": 144323921,
          "CPZLMessage": {
            "Template": "{0} 重量{1}",
            "ShowConfig": {
              "FontSize": 0,
              "ShowStyle": 3,
              "ShowSpeed": 8
            }
          },
          "WelcomeMessage": {
            "Template": "欢迎称重",
            "ShowConfig": {
              "FontSize": 1,
              "ShowStyle": 3,
              "ShowSpeed": 8
            }
          }
        },
        "DownRelay": {
          "IP": "192.168.1.199",
          "Port": 12345,
          "OutGroups": [
            {
              "SuccessOut": 1,
              "FailureOut": 2
            },
            {
              "SuccessOut": 3,
              "FailureOut": 4
            },
            {
              "SuccessOut": 5,
              "FailureOut": 6
            }
          ]
        }
      },
      "Exit": {
        "Name": "出口",
        "Button": 5,
        "Gratings": {
          "Front": 7,
          "Back": 8
        },
        "ScaleIP": "192.168.1.111",
        "InOrOut": 0,
        "Timeout": 5000,
        "LED": {
          "IP": "192.168.1.222",
          "MaterialUid": 144323921,
          "CPZLMessage": {
            "Template": "{0} 重量{1}",
            "ShowConfig": {
              "FontSize": 0,
              "ShowStyle": 3,
              "ShowSpeed": 8
            }
          },
          "WelcomeMessage": {
            "Template": "欢迎称重",
            "ShowConfig": {
              "FontSize": 1,
              "ShowStyle": 3,
              "ShowSpeed": 8
            }
          }
        },
        "DownRelay": {
          "IP": "192.168.1.199",
          "Port": 12345,
          "OutGroups": [
            {
              "SuccessOut": 1,
              "FailureOut": 2
            },
            {
              "SuccessOut": 3,
              "FailureOut": 4
            },
            {
              "SuccessOut": 5,
              "FailureOut": 6
            }
          ]
        }
      }
    }
  ]


json-server -m service-middleware.js -w test-service.json