#include "LCDModule.h"

LCDModule::LCDModule(std::string name, SerialController serialController, Buzzer& buzzer) : Component(name), serialController(serialController), buzzer(buzzer){
  lcd.init();
}


void LCDModule::LCDSetup() {

  lcd.setDelay(0, 0);
  
  lcd.createChar(0, upArrow);

  lcd.createChar(1, downArrow);

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

void LCDModule::refresh(){
  lcd.init();
  changed = true;
}

void LCDModule::backLightOff(){
  lcd.noBacklight();
}

void LCDModule::backLightOn(){
  lcd.backlight();
}

void LCDModule::set(std::vector<std::string> args){

  std::vector<std::string> formattedArgs;
  std::string buffer;
  bool inQuotes = false;

  if(args.size() == 1 && std::stoi(args[0]) == 0){
    topText = "";
    bottomText = "";
    changed = true;
    return;
  }

  for (const std::string &arg : args) {

      bool startsWithQuote = arg.front() == '"';
      bool endsWithQuote   = arg.back() == '"';

      if (!inQuotes) {
          if (startsWithQuote && !endsWithQuote) {
              inQuotes = true;
              buffer = arg;
          }
          else if (startsWithQuote && endsWithQuote) {
              formattedArgs.push_back(arg);
          }
          else {
              formattedArgs.push_back(arg);
          }
      }
      else {
          buffer += " " + arg;
          if (endsWithQuote) {
              inQuotes = false;
              formattedArgs.push_back(buffer);
              buffer.clear();
          }
      }
  }

  if(!(topText == "Unknown Error!!!" && bottomText == "Power Cycle Req.")){
    tempTopText = topText;
    tempBottomText = bottomText;

    changed = true;
    if(formattedArgs.size() == 1){
      topText = formattedArgs[0];
      topText.erase(remove( topText.begin(), topText.end(), '\"' ),topText.end());
    }else if(formattedArgs.size() == 2){
      topText = formattedArgs[0];
      bottomText = formattedArgs[1];
    
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

  for(auto& ch : topText){
    if(ch == '\x00') {lcd.write((char)0); continue;;}
    if(ch == '\x01') {lcd.write((char)1); continue;}
    if(ch == '\x02') {lcd.write('\x7F'); continue;}
    if(ch == '\x03') {lcd.write('\x7E'); continue;}
    lcd.write(ch);
  }

  lcd.setCursor(0, 1);

  for(auto& ch : bottomText){
    if(ch == '\x00') {lcd.write((char)0); continue;;}
    if(ch == '\x01') {lcd.write((char)1); continue;}
    if(ch == '\x02') {lcd.write('\x7F'); continue;}
    if(ch == '\x03') {lcd.write('\x7E'); continue;}
    lcd.write(ch);
  }

}