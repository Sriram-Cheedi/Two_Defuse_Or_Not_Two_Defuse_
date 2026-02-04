#pragma once
#include <Arduino.h>
#include <Component.h>
#include <SerialController.h>
#include <TeensyThreads.h>
#include "Button.h"

class SimonSaysModule : public Component {

private:

    const std::string name;

    const std::vector<int> buttonPins;
    std::vector<Button> buttons;

    int state;  // 0 = Not Started, 1 = In Progress, 2 = Complete, 3 = Failed (Resetting)

    int threadID = -1;

    size_t currentRound;
    std::vector<int> correctSequence;
    std::vector<int> inputSequence;

    std::string pressedButton;

    void reset();     // Reset Module to the initial state
    void complete();  // Auto complete the module 
    
    void buttonPressed(std::string btnName, int state);

    void loop();
    int getInputSequence();

    //void set_leds(int* rgb1, int* rgb2, int* rgb3, int* rgb4);

    SerialController serialController;

        static void loopTrampoline(void *arg) {
            SimonSaysModule *self = static_cast<SimonSaysModule*>(arg);
            self->loop();   // call the real method
        }

public:

    SimonSaysModule(std::string name, std::vector<int> buttonPins, SerialController serialController);

    void set(std::vector<std::string> args) override;
    void status() override;

};