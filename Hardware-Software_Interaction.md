## Communication Protocol Unity -> Arduino

$header:cmd#

Header: code of asynchronous minigame or syncronous control

cmd: function x y z ... 

## Timer

| Command            | Code            | Values                              | Response             | Values                  | Description                                                                  |
|--------------------|-----------------|-------------------------------------|----------------------|-------------------------|------------------------------------------------------------------------------|
| Set remaining time | $TMR:set x#     | x: Time in seconds                  | $TMR:status y#        | y: Time in seconds      | Sets the timer to the equivalent MM:SS format for the given time in seconds. |
| Flash timer        | $TMR:flash x y# | x: Number of flashes y: Flash delay | $TMR:status flashing# | y: Time in seconds      | Flashes all the digits of the timer                                          |
| Set timer on/off   | $TMR:on x#      | x: 1 = on, 0 = off                  | $TMR:status x#        | x: -1 if off, >=0 if on | Turns the timer display on/off                                               |

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

