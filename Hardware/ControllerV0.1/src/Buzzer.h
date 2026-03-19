#pragma once
#include "Component.h"
#include "SerialController.h"
#include <functional>

class Buzzer : public Component{

private:

    const int pin;
    SerialController& serialController;

    int state = 0;

    const unsigned long delay = 0;
    unsigned long lastBeep = 0;

    bool enabled = true;
    bool siren = false;
    int pitch = 50;

    bool rise = true;

public:

    void scramble() override{
        disable();
    };

    void sirenOn();

    void sirenOff();

    void enable() override;
    void disable() override;
    void status() override;
    void set(std::vector<std::string> args) override;
    void update() override;

    Buzzer(std::string name, int pin, SerialController serialController, int pitch);

};