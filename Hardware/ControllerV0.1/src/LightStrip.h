#pragma once
#include <array>
#include <stdint.h>
#include <string>
#include "component.h"
#include "serialController.h"
#include "LCDModule.h"
#include "Button.h"
#include "WS2812Serial.h"

class LightStrip : public Component {

private:

    int ledCount;

    uint32_t* drawingMemory;
    byte* displayMemory;

    bool changed = false;
    bool scrambled = false;

    WS2812Serial strip;
    std::vector<uint32_t> pixelBuffer;

    uint32_t lastScrambleUpdate = 0;

    std::vector<uint32_t> colourMap = {Color(255, 255, 0), Color(0, 255, 0), Color(255, 0, 0), Color(0, 0, 255)};

public:

    LightStrip(std::string name, int pin, int ledCount);

    static inline uint32_t Color(uint8_t r, uint8_t g, uint8_t b) {
        return ((uint32_t)r << 16) | ((uint32_t)g << 8) | b;
    }

    void setAll(uint32_t color);
    void setPixel(int index, uint32_t color);
    void set(std::vector<std::string> args) override;
    void status() override;
    void disable() override;
    void enable() override;
    void update() override;
    void scramble() override;

};
