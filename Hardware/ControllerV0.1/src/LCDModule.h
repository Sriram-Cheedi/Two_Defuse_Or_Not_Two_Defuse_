#ifndef LCDMODULE_H
#define LCDMODULE_H

#include <Arduino.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include <string>
#include <Component.h>
#include <SerialController.h>

class LCDModule : public Component {

    SerialController serialController;

    std::string topText = "";
    std::string bottomText  = "";

    std::string tempTopText = "";
    std::string tempBottomText = "";

    std::vector<std::string> textList = std::vector<std::string>();

    bool enabled = true;
    bool changed = false;
public:
    LCDModule(std::string name, SerialController serialController);
    void LCDSetup();
    void LCDScroll();
    void LCDSet(char word[]);

    void enable() override;
    void disable() override;
    void set(std::vector<std::string> args) override;
    void status() override;
    void update() override;
    void scramble() override{
        disable();
    };

};

#endif