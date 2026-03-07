#pragma once
#include "Component.h"
#include "SerialController.h"
#include <functional>

class Button : public Component{

private:

    const int pin;
    SerialController& serialController;
    std::function<void (std::string, int)> onPressed;

    int lastState;
    bool enabled = false;

public:

    void enable() override;
    void disable() override;
    void status() override;
    void set(std::vector<std::string> args) override;

    Button(std::string name, int pin, SerialController serialController);
    Button(std::string name, int pin, SerialController serialController, std::function<void (std::string, int)> onPressed);

    void update();

};