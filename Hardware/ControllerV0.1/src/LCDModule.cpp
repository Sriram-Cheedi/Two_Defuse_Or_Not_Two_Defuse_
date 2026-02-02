#include <Arduino.h>
#include <Wire.h>
#include "LCDModule.h"
#include <LiquidCrystal_I2C.h>

LCDModule::LCDModule(){
  
}

LiquidCrystal_I2C lcd(0x20,16,2); 

void LCDModule::LCDSetup() {
  lcd.init();                      // initialize the lcd

  // Print a message to the LCD.
  lcd.backlight();
  lcd.print("Hello, world!");
}
 
void LCDModule::LCDLoop() {
}