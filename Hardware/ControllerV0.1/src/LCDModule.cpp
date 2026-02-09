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
  //lcd.print("Guess the Number using the four buttons. The 5th button will submit your current guess. No light indicates the number is not correct. Yellow light indicates that the number is correct but in the wrong position. Green is the correct number in the correct place. Numbers do no repeat.");
  lcd.print("Guess the Number");
}

void LCDModule::LCDScroll(){
  lcd.scrollDisplayLeft();
}

void LCDModule::LCDSet(char word[]){
  lcd.clear();
  lcd.backlight();
  lcd.print(word);
}