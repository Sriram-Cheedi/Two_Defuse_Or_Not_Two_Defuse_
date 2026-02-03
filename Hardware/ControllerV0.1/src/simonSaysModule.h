#pragma once
#include <Arduino.h>
#include <Component.h>
#include <SerialController.h>

class SimonSaysModule : public Component {

    const std::string name;

    const int dataPin;
    const int clockPin;
    const int latchPin;

    int state;  // 0 = Not Started, 1 = In Progress, 2 = Complete, 3 = Failed (Resetting)

    void reset();     // Reset Module to the initial state
    void complete();  // Auto complete the module 
    
    void set_leds(int* rgb1, int* rgb2, int* rgb3, int* rgb4);

    SerialController serialController;

public:

    SimonSaysModule(std::string name, int dataPin, int clockPin, int latchPin, SerialController serialController);

    void set(std::vector<std::string> args) override;
    void status() override;

};