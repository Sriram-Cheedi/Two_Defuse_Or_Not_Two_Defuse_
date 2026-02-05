#include <stdint.h>
#include <string>
#include "component.h"
#include "serialController.h"
#include "LCDModule.h"

class numberGameModule : public Component {

private:
    const int dataPin;
    const int clockPin;
    const int latchPin;

    const int buttonPin;
    int buttonPushCounter = 0;
    int buttonState = 0;
    float lastButtonState = 0;
    float lastVoltage = 0;

    uint8_t ledStates = 0b00000000;
    int secretNumber[4];
    byte attempts = 0;
    const byte ledPositions[4][2] = {
        {7, 0},
        {1, 2},
        {3, 4},
        {5, 6}
    };

    SerialController serialController;
    LCDModule& display;

    unsigned long displayUpdate = 0;
    unsigned long lastUpdate = 0;
    unsigned long buttonUpdate = 0;
    const unsigned int refreshInterval = 200;
    uint8_t currentDigit = 0; 
    int digitsBuffer[4] = {0,0,0,0};

    /*const uint8_t segment_encodings[10] = {
        0b11111100, // 0
        0b01100000, // 1
        0b11011010, // 2
        0b11110010, // 3
        0b01100110, // 4
        0b10110110, // 5
        0b10111110, // 6
        0b11100000, // 7
        0b11111110, // 8
        0b11100110  // 9
    };

    const uint8_t digit_encodings[5] = {
        0b01110000, // first digit
        0b10110000, // second digit
        0b11010000, // third digit
        0b11100000, // fourth digit
    };*/

    const uint8_t segment_encodings[11] = {
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

    numberGameModule(std::string name, int dataPin, int clockPin, int latchPin, int buttonPin,SerialController serialController,LCDModule& display);
    void set(std::vector<std::string> args) override;
    void status() override;
    void setupGame();
    void checkGuess();
    void buttonCheck();
    void updateDisplay();
  
};