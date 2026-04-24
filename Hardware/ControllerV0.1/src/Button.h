#pragma once
#include "Component.h"
#include "SerialController.h"
#include <functional>

class Button : public Component{

private:

    const int pin;
    SerialController& serialController;
    std::function<void (std::string, int)> onPressed;

    const uint32_t debounceDelay = 50;
    uint32_t lastDebounceTime = 50;

    int lastState = -1;

    bool enabled = true;


public:

    int buttonState = -1;

    void scramble() override{
        disable();
    };
    void enable() override;
    void disable() override;
    void status() override;
    void set(std::vector<std::string> args) override;

    Button(std::string name, int pin, SerialController serialController);
    Button(std::string name, int pin, SerialController serialController, std::function<void (std::string, int)> onPressed);

    void update() override;

};