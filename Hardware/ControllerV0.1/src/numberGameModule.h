#ifndef NUMBERGAMEMODULE_H
#define NUMBERGAMEMODULE_H

#include <array>
#include <stdint.h>
#include <string>
#include "component.h"
#include "serialController.h"
#include "LCDModule.h"
#include "Button.h"

class numberGameModule : public Component {

private:
    int state = 0; // 0 = In Progress, 1 = Submission, 2 = Complete

    const int dataPin;
    const int clockPin;
    const int latchPin;
    const std::array<int,5> buttonPins; //First 4 button are used to increment the display numbers. Button 5 will be the submit button.
    std::vector<Button> buttons;
    int secretNumber[4];

    SerialController serialController;

    unsigned long lastUpdate = 0;
    unsigned long buttonUpdate = 0;
    unsigned long submitUpdate = 0;
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

    numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, std::array<int,5> buttonPins, SerialController serialController);
    void set(std::vector<std::string> args) override;
    void status() override;
    void setupGame();
    void checkGuess(std::string btnName, int btnState);
    void buttonCheck(std::string btnName, int btnState);
    void update();
  
};

#endif