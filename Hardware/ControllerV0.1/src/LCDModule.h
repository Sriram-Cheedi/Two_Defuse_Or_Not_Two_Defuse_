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

    LiquidCrystal_I2C lcd = LiquidCrystal_I2C(0x20,16,2); 

    byte upArrow[8] = {
        0x00,
        0x04,  //   *  
        0x0E,  //  *** 
        0x15,  // * * *
        0x04,  //   *  
        0x04,  //   *  
        0x04,  //   *  
        0x00
    };  

    byte downArrow[8] = {
        0x00,
        0x04,  //   *  
        0x04,  //   *  
        0x04,  //   *  
        0x15,  // * * *
        0x0E,  //  *** 
        0x04,  //   * 
        0x00 
    };

    std::string topText = "";
    std::string bottomText  = "";

    std::string tempTopText = "";
    std::string tempBottomText = "";

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