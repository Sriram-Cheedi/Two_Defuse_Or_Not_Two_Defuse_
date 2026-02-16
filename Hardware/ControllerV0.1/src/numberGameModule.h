#ifndef NUMBERGAMEMODULE_H
#define NUMBERGAMEMODULE_H

#include <array>
#include <stdint.h>
#include <string>
#include "component.h"
#include "serialController.h"
#include "LCDModule.h"

class numberGameModule : public Component {

private:
    int state = 0; // 0 = Not Started, 1 = In Progress, 2 = Complete, 3 = Failed (Resetting)

    const int dataPin;
    const int clockPin;
    const int latchPin;
    const std::array<int,5> buttonPins; //First 4 button are used to increment the display numbers. Button 5 will be the submit button.

    int buttonPushCounter = 0;
    int buttonState = 0;
    //int lastButtonState = 0;
    std::array<int,5> lastInputStates = {0,0,0,0,0};

    uint8_t ledStates = 0b00000000;
    int secretNumber[4];
    const byte ledPositions[4][2] = {
        {7, 6},
        {5, 4},
        {3, 2},
        {1, 0}
    };

    SerialController serialController;
    LCDModule& display;

    unsigned long lastUpdate = 0;
    unsigned long buttonUpdate = 0;
    const unsigned int refreshInterval = 200;
    uint8_t currentDigit = 0; 
    int digitsBuffer[4] = {0,0,0,0};

    //common anode segment encoding
    const uint8_t segment_encodings[10] = {
        0b00000011, // 0
        0b10011111, // 1
        0b00100101, // 2
        0b00001101, // 3
        0b10011001, // 4
        0b01001001, // 5
        0b01000001, // 6
        0b00011111, // 7
        0b00000001, // 8
        0b00011001,  // 9
    };

    const uint8_t digit_encodings[4] = {
        0b10000000, // first digit
        0b01000000, // second digit
        0b00100000, // third digit
        0b00010000, // fourth digit
    };

public:

    numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, std::array<int,5> buttonPins, SerialController serialController, LCDModule& display);
    void set(std::vector<std::string> args) override;
    void status() override;
    void setupGame();
    void checkGuess();
    void buttonCheck();
    void updateDisplay();
  
};

#endif