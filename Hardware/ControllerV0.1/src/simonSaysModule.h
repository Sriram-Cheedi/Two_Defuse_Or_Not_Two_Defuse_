#pragma once
#include <Arduino.h>
#include <Component.h>
#include <SerialController.h>
#include <TeensyThreads.h>
#include <atomic>
#include "Button.h"
#include "WS2812Serial.h"

class SimonSaysModule : public Component {

    static inline uint32_t Color(uint8_t r, uint8_t g, uint8_t b) {
        return ((uint32_t)r << 16) | ((uint32_t)g << 8) | b;
    }

private:

    // MAIN THREAD ONLY VARIABLES

    const std::vector<int> buttonPins;
    std::vector<Button> buttons;
    const int rgbPin;

    int oldState = 0;
    int threadID = -1;

    SerialController serialController;

    byte drawingMemory[16*3];
    byte displayMemory[16*12];

    WS2812Serial strip = WS2812Serial(4, drawingMemory, displayMemory, 29, WS2812_GRB);

    std::vector<uint32_t> colourMap = {Color(255, 255, 0), Color(0, 255, 0), Color(255, 0, 0), Color(0, 0, 255)};

    // SSM THREAD ONLY VARIABLES
    size_t currentRound;

    // MULTI-THREAD VARIABLES (Protected) 
    bool enabled = true;
    std::vector<uint32_t> pixelBuffer = {Color(255, 0, 0), Color(255, 0, 0), Color(255, 0, 0), Color(255, 0, 0)}; 
    std::atomic<int> state{0};  // 0 = Not Started, 1 = In Progress, 2 = Complete, 3 = Failed (Resetting)
    std::atomic<int> pressedButton{-1};
    std::vector<int> correctSequence = {0, 1, 2, 3};

    Threads::Mutex resetingMutex; //Protects Correct Sequence
    Threads::Mutex pixelBufferMutex; //Protects pixelBuffer
    // --------------------------

    // EXTERNAL THREAD FUNCTIONS
    void off();   // Turn off module
    void reset();     // Reset Module to the initial state
    void complete();  // Auto complete the module 
    void buttonPressed(std::string btnName, int state);

    // SSM THREAD ONLY FUNCTIONS
    void loop();
    int getInputSequence();
    void completeSequence();
    void setPixelBuffer(std::vector<uint32_t> newColours);
    void setPixelBuffer(int index, uint32_t newColour);
    void set_leds(std::vector<int> rgb1, std::vector<int> rgb2, std::vector<int> rgb3, std::vector<int> rgb4);

    // Static function to being SSM thread
    static void loopTrampoline(void *arg) {
        SimonSaysModule *self = static_cast<SimonSaysModule*>(arg);
        self->loop();   // call the real method
    }

public:

    SimonSaysModule(std::string name, std::vector<int> buttonPins, int rgbPin, SerialController serialController);

    void update() override; //Called from main loop
    void enable() override;
    void disable() override;
    void set(std::vector<std::string> args) override; //Called from serialController (main loop)
    void status() override; //Called from serialController (main loop)

};