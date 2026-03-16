#include "LCDModule.h"

LCDModule::LCDModule(std::string name, SerialController serialController) : Component(name), serialController(serialController){
  
}

LiquidCrystal_I2C lcd(0x20,16,2); 

void LCDModule::LCDSetup() {
  lcd.init();                      // initialize the lcd

  lcd.backlight();
}

void LCDModule::LCDScroll(){
  lcd.scrollDisplayLeft();
}

void LCDModule::LCDSet(char word[]){
  lcd.clear();
  lcd.backlight();
  lcd.print(word);
}

void LCDModule::enable(){
  enabled = true;
  changed = true;

  if(topText == "Unknown Error!!!" && bottomText == "Power Cycle Req."){
    topText = tempTopText;
    bottomText = tempBottomText;
  }
}

void LCDModule::disable(){

  enabled = false;
  lcd.clear();
  lcd.noBacklight();
}

void LCDModule::set(std::vector<std::string> args){
  if(!(topText == "Unknown Error!!!" && bottomText == "Power Cycle Req.")){
    tempTopText = topText;
    tempBottomText = bottomText;

    changed = true;
    if(args.size() == 1){
      topText = args[0];
      topText.erase(remove( topText.begin(), topText.end(), '\"' ),topText.end());
    }else if(args.size() == 2){
      topText = args[0];
      bottomText = args[1];
    
      topText.erase(remove( topText.begin(), topText.end(), '\"' ),topText.end());
      bottomText.erase(remove( bottomText.begin(), bottomText.end(), '\"' ),bottomText.end());
    }
  }else{
    tempTopText = args[0];
    tempBottomText = args[1];
  }
  

  
}

void LCDModule::status(){
  std::string state = "\"" + topText + "\" \"" + bottomText + "\"";
  serialController.serialWrite(name, state);
}

void LCDModule::update(){
  
  if(!changed) return;
  changed = false;

  if(!enabled) return;

  lcd.clear();
  lcd.backlight();
  lcd.print(topText.c_str());
  lcd.setCursor(0, 1);
  lcd.print(bottomText.c_str());
}