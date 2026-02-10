#pragma once
#include <Arduino.h>
#include <component.h>
#include <SerialController.h>

class fishingModule : public Component {
    const std::string name;

    const int dataPin;
    const int clockPin;
    const int latchPin;

    int state; // 0 = Not Started, 1 = In Progress, 2 = Complete, 3 = Failed (Resetting)

    SerialController serialController;

public:

    fishingModule(std::string name, int dataPin, int clockPin, int latchPin, SerialController SerialController);
    void set(std::vector<std::string> args) override;
    void status() override;
};