#pragma once
#include "Component.h"
#include "SerialController.h"
#include <functional>

class Buzzer : public Component{

private:

    const int pin;
    SerialController& serialController;

    int state = 0;

    int winThread;

    const unsigned long beepDelay = 10;
    unsigned long lastBeep = 0;

    const unsigned long notificationDelay = 1000;
    unsigned long lastNotification = 0;

    unsigned long delay = 500;
    unsigned long lastDelay = 0;

    unsigned long lastScramble = 0;
    unsigned long scrambleDelay = 50;
    bool scrambleRise = false;

    int duration = 0;
    int beepNum = 0;

    bool enabled = true;
    bool siren = false;
    bool notificationPlay = false;
    bool scrambleNoise = false;
    bool pause = false;
    bool winNoise = false;
    bool loseNoise = false;
    bool rr = false;

    int pitch = 50;

    bool rise = true;
    bool onChange = true;


public:

    void scramble() override;

    void sirenOn();

    void sirenOff();

    void notification();

    void win();

    void lose();

    void rickRoll();

    static void playWinNoise();

    void enable() override;
    void disable() override;
    void status() override;
    void set(std::vector<std::string> args) override;
    void update() override;

    Buzzer(std::string name, int pin, SerialController serialController, int pitch);

};