## Communication Protocol Unity -> Arduino

$header:cmd#

Header: code of asynchronous minigame or syncronous control

cmd: function x y z ... 

## Timer

| Command                 | Code                                                 | Response               | Notes                                                                                   |
|-------------------------|------------------------------------------------------|------------------------|-----------------------------------------------------------------------------------------|
| Get displayed time left | $TMR:status#                                         | $X:status _time-left_# |                                                                                         |
| Set time left           | $TMR:set _time-left_#                                | None                   |                                                                                         |
| Set displayed digits    | $TMR:set _digit1_ _digit2_ _digit3_ _digit4_ _colon_ | None                   | Valid digits: 0, ..., 10 where 10 is digit off <br> Colon: "true" is on, "false" is off |

Status of timer is the number of seconds it is displaying, unless display is turned off where the status is -1.

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

