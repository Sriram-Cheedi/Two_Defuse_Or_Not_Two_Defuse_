#pragma once
#include <Arduino.h>
#include <Component.h>
#include <SerialController.h>
#include <TeensyThreads.h>
#include "Button.h"
#include "Adafruit_NeoPixel.h"

class SimonSaysModule : public Component {

private:

    const std::string name;

    const std::vector<int> buttonPins;
    std::vector<Button> buttons;

    std::vector<uint32_t> pixelBuffer; 

    const int rgbPin;

    int state;  // 0 = Not Started, 1 = In Progress, 2 = Complete, 3 = Failed (Resetting)

    int threadID = -1;

    std::mutex pixelBufferMutex;

    size_t currentRound;
    std::vector<int> correctSequence;
    std::vector<int> inputSequence;

    std::string pressedButton;

    Adafruit_NeoPixel strip;

    void reset();     // Reset Module to the initial state
    void complete();  // Auto complete the module 
    
    void buttonPressed(std::string btnName, int state);

    void loop();
    int getInputSequence();

    void setPixelBuffer(std::vector<uint32_t> newColours);

    void set_leds(std::vector<int> rgb1, std::vector<int> rgb2, std::vector<int> rgb3, std::vector<int> rgb4);

    SerialController serialController;

        static void loopTrampoline(void *arg) {
            SimonSaysModule *self = static_cast<SimonSaysModule*>(arg);
            self->loop();   // call the real method
        }

public:

    SimonSaysModule(std::string name, std::vector<int> buttonPins, int rgbPin, SerialController serialController);

    void update();
    void init();

    void set(std::vector<std::string> args) override;
    void status() override;

};