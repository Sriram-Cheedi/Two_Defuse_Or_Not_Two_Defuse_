#pragma once
#include "Component.h"
#include "SerialController.h"
#include <functional>

class Button : public Component{

private:

    const int pin;
    SerialController& serialController;
    std::function<void (std::string, int)> onPressed;

    uint32_t debounceDelay;
    uint32_t lastDebounceTime = 0;

    int lastState = -1;

    bool enabled = true;


public:

    int buttonState = -1;

    void reset();

    void scramble() override{
        disable();
    };
    void enable() override;
    void disable() override;
    void status() override;
    void set(std::vector<std::string> args) override;

    Button(std::string name, int pin, SerialController serialController, uint32_t debounceTime);
    Button(std::string name, int pin, SerialController serialController, std::function<void (std::string, int)> onPressed, uint32_t debounceTime);

    void update() override;

};