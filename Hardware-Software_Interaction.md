## Communication Protocol Unity -> Arduino

$header:cmd#

Header: code of asynchronous minigame or syncronous control

cmd: function x y z ... 

## Timer

| Command                 | Code                                                 | Response               | Notes                                                                                   |
|-------------------------|------------------------------------------------------|------------------------|-----------------------------------------------------------------------------------------|
| Get displayed time left | $TMR:status#                                         | $X:status _time-left_# |                                                                                         |
| Set time left           | $TMR:set _time-left_#                                | None                   |                                                                                         |
| Set displayed digits    | $TMR:set _digit1_ _digit2_ _digit3_ _digit4_ _colon_# | None                   | Valid digits: 0, ..., 10 where 10 is digit off <br> Colon: "true" is on, "false" is off |


## IMU

| Command           | Code         | Response                                                        | Notes                                                                         |
|-------------------|--------------|-----------------------------------------------------------------|-------------------------------------------------------------------------------|
| Get status of IMU | $IMU:status# | $IMU:status _gyroX_ _gyroY_ _gyroZ_ _accelX_ _accelY_ _accelZ_# | Response will be sent when requested OR whenever movement is detected by IMU. |

## Simon Says

| Command                            | Code                | Response             | Notes                                                                                  |
|------------------------------------|---------------------|----------------------|----------------------------------------------------------------------------------------|
| Get Status of Simon Says           | $SSM:status#        | $SSM:status _state_# | State: 0 = Off, 1 = In Progress, 2 = Complete                                  |
| Set Status of Simon Says           | $SSM:set _state_#   | None                 | See above                                                                              |
| Reset Simon Says with new sequence | $SSM:set _sequence_#| None                | Sequence of format "seq1 seq2 seq3 seq4 ..." where each element is a single digit 0-3 e.g. $SSM:set 0 1 2 3#. Sets state to 0. |

## Number Game

| Command                            | Code                | Response             | Notes                                                                                  |
|------------------------------------|---------------------|----------------------|----------------------------------------------------------------------------------------|
| Get Status of number game          | $NGM:status#        | $NGM:status _submitting_ _dig1_ _dig2_ _dig3_ _dig4_# | Submitting: 0 = no, 1 = yes                           |
| Set Status of number game          | $NGM:set _state_#   | None                 | State: 0 = Off, 1 = On                                                                              |
| Reset number game with new sequence| $NGM:set _sequence_#| None                 | Sequence of format "dig1 dig2 dig3 dig4" where each element is a single digit 0-9. Sets state to 0. |

## Fishing Game

| Command                            | Code                | Response             | Notes                                                                                  |
|------------------------------------|---------------------|----------------------|----------------------------------------------------------------------------------------|
| Get Status of fishing game          | $FGM:status#        | $FGM:status _status_# | Status: 0 = Off, 1 = On (In correct position), 2 = On (In incorrect position)                           |
| Set Status of fishing game          | $FGM:set _state_#   | None                 | State: 0 = Off, 1 = On                                                                              |

## LCD Module

| Command                            | Code                | Response             | Notes                                                                                  |
|------------------------------------|---------------------|----------------------|----------------------------------------------------------------------------------------|
| Get Status of LCD          | $LCD:status#        | $LCD:status "_TopLine_" "_BottomLine_"# | Quotes are included in response surrounding each argument                   |
| Set Status of LCD          | $LCD:set "_TopLine_" "_BottomLine_"#   | None                 | Quotes are included in response surrounding each argument                   |

## Button Component

| Command                            | Code                | Response             | Notes                                                                                  |
|------------------------------------|---------------------|----------------------|----------------------------------------------------------------------------------------|
| Get Status of button               | $_BTN_:status#      | $_BTN_:status _state_# | BTN: name of button. State: 0 = Unpressed, 1 = Pressed                               |

## Bomb Power

| Command                            | Code                | Response             | Notes                                                                                  |
|------------------------------------|---------------------|----------------------|----------------------------------------------------------------------------------------|
| Get Status of power          | $PWR:status#        | $PWR:status _state_# | 0 = Not Powered, 1 = Powered, 3 = Power Cycle Minigame, 4 = Disable, 5 = Enable              |
| Set Status of power          | $PWR:set _state_#   | None                 | See above                   |

## Global Status Request

| Command                            | Code                | Response             | Notes                                                                                  |
|------------------------------------|---------------------|----------------------|----------------------------------------------------------------------------------------|
| Get Status of all components       | $GLB:status#        | Status Response from all components |                                                               |

## Asynchronous Minigames

Asyncronous minigames' logic will be onboard. Status of the minigame can be queried through the following commands. Any changes made to the hardware of the minigame **will not** trigger a status update. Changes of the state of the minigame (not started, in progress, failed, completed) **will** trigger a status update without requiring a request. The meaning of status is dependant on the minigame.

| Command        | Code       | Values                                          | Response     | Values                | Description                                                                                 |
|----------------|------------|-------------------------------------------------|--------------|-----------------------|---------------------------------------------------------------------------------------------|
| Status Request | $X:status# | X: Code of minigame                             | $X:status y# | y: Status of Minigame | Requests the status of the Minigame                                                         |
| Set Status     | $X:set y#  | X: Code of minigame, y: new status for minigame | $X:status y# | y: Status of Minigame | Sets the status of the Minigame, exact effect and implementation dependent on the minigame. |

## Synchronous Minigames

Syncronous minigames will give hardware level access to unity through the following commands. Any changes made to the hardware component will trigger a status response without requiring a request. Reset will do its best to reset the status of any components although physical switches will not be effected. The format of status is dependant on the hardware component (Switches/Buttons - binary, Display - string).

| Command        | Code       | Values                                       | Response     | Values                                       | Description                                                                                                             |
|----------------|------------|----------------------------------------------|--------------|----------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| Status Request | $X:status# | X: Code of hardware component                | $X:status y# | X: Code of hardware component, y: Status     | Requests the status of the hardware component, response value can be in any format according to the component requested |
| Set Value      | $X:set y#  | X: Code of hardware component, y: New status | $X:status y# | X: Code of hardware component, y: New status | Sets the state of the hardware component, if setting is not possible (physical switches) nothing will happen.           |

