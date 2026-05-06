#pragma once
#include <array>
#include <stdint.h>
#include <string>
#include "component.h"
#include "serialController.h"
#include "LCDModule.h"
#include "Button.h"
#include "Buzzer.h"

class MazeModule : public Component {

    SerialController serialController;
    LCDModule& lcd;
    Buzzer& buzzer;

    int32_t lastUpdate = 0;

    int32_t refreshInterval = 3000;

    bool enabled = true;
    std::vector<std::vector<int>> route = {};
    int state = 0;

    int currentPage = 0;
    int pageLength = 0;

    public:

    MazeModule(std::string name, SerialController serialController, LCDModule& lcd, Buzzer& buzzer);
    void set(std::vector<std::string> args) override;
    void status() override;
    void disable() override;
    void enable() override;
    void update() override;
    void scramble() override;

};
