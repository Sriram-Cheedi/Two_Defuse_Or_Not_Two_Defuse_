#include <stdint.h>
#include <string>
#include "Component.h"
#include "SerialController.h"

class TimerModule : public Component {

private:

    const int dataPin;
    const int clockPin;
    const int latchPin;

    int seconds;

    SerialController serialController;

    unsigned long lastUpdate = 0;
    const unsigned int refreshInterval = 10;
    uint8_t currentDigit = 0;
    bool colonEnabled = false;
    int digitsBuffer[4];

    bool enabled = true;
    
    const uint8_t segment_encodings[11] = {
        0b11111100, // 0
        0b01100000, // 1
        0b11011010, // 2
        0b11110010, // 3
        0b01100110, // 4
        0b10110110, // 5
        0b10111110, // 6
        0b11100000, // 7
        0b11111110, // 8
        0b11100110, // 9
        0b00000000  // off
    };

    const uint8_t digit_encodings[5] = {
        0b01110000, // first digit
        0b10110000, // second digit
        0b11010000, // third digit
        0b11100000, // fourth digit
        0b11111000  // colon
    };

public:

    void enable() override;
    void disable() override;

    TimerModule(std::string name, int dataPin, int clockPin, int latchPin, SerialController serialController);
    void set(std::vector<std::string> args) override;
    void status() override;

    void setDigits(int* digits, bool colon);
    void update() override;
  
};