#pragma once
#include <Arduino.h>
#include <component.h>
#include <SerialController.h>
#include "Button.h"

class fishingModule : public Component {

    const int dataPin;
    const int clockPin;
    const int latchPin;
    const int buttonPin;

    int hookPos = 9;
    int points = 0;
    long randState = 0;

    const unsigned int gameInterval = 500;
    unsigned long lastGameUpdate = 0;

    const unsigned int fishInterval = 2000;
    unsigned long lastFishUpdate = 0;

    const unsigned int hookInterval = 500;
    unsigned long lastHookUpdate = 0;

    const unsigned int scoreInterval = 500;
    unsigned long lastScoreUpdate = 0;

    const unsigned int buttonInterval = 50;
    unsigned long lastButtonUpdate = 0;

    
    uint32_t hookLED [10] = {
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

    uint32_t fishLED [12] = {
        0b0000000001,
        0b0000000011,
        0b0000000111,
        0b0000001110,
        0b0000011100,
        0b0000111000,
        0b0001110000,
        0b0011100000,
        0b0111000000,
        0b1110000000,
        0b1100000000,
        0b1000000000
    };

    int state = 0; // 0 = Off, 1 = On correct position, 2 = On incorrect position

    SerialController serialController;
    Button btn;

    void reset();     // Reset Module to the initial state
    void complete();  // Auto complete the module 
    void buttonUpdate(std::string btnName, int btnState);
    void shift(uint32_t digit);
    void scoreCheck();

public:

    fishingModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin, SerialController SerialController);
    void set(std::vector<std::string> args) override;
    void status() override;

    void update();
};