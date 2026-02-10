#pragma once
#include <Arduino.h>
#include <component.h>
#include <SerialController.h>

class fishingModule : public Component {
    const std::string name;

    const int dataPin;
    const int clockPin;
    const int latchPin;
    const int buttonPin;

    int buttonState = 0;
    int lastButtonState = 0;
    int fishPos = 9;

    const unsigned int gameInterval = 200;
    unsigned long lastGameUpdate = 0;

    const unsigned int fishInterval = 1000;
    unsigned long lastFishUpdate = 0;

    const unsigned int hookInterval = 100;
    unsigned long lastHookUpdate = 0;

    int points = 0;
    int randState = 0;
    
    uint32_t fishLED [10] = {
        0b1000000000,
        0b0100000000,
        0b0010000000,
        0b0001000000,
        0b0000100000,
        0b0000010000,
        0b0000001000,
        0b0000000100,
        0b0000000010,
        0b0000000001
    };

    uint32_t pondLED [12] = {
        0b1000000000,
        0b1100000000,
        0b1110000000,
        0b0111000000,
        0b0011100000,
        0b0001110000,
        0b0000111000,
        0b0000011100,
        0b0000001110,
        0b0000000111,
        0b0000000011,
        0b0000000001
    };

    int state; // 0 = Not Started, 1 = In Progress, 2 = Complete, 3 = Failed (Resetting)

    SerialController serialController;

public:

    fishingModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin, SerialController SerialController);
    void set(std::vector<std::string> args) override;
    void status() override;

    void updateGame();
private:
    void shift(uint32_t digit);
    void scoreCheck();
};