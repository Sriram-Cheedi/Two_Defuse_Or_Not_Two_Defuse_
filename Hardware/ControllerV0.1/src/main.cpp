#include <Arduino.h>
#include <vector>
#include <bits/stdc++.h>
#include "timerModule.h"
#include "component.h"
#include "serialController.h"
#include "numberGameModule.h"
#include "LCDModule.h"


SerialController serialController = SerialController();

timerModule timer = timerModule("TMR", 23, 24, 25, serialController);

numberGameModule numberGame = numberGameModule("NGM",36,34,35,A8,serialController);

LCDModule LCDDisplay = LCDModule();

std::vector<std::unique_ptr<Component>> components;

std::vector<std::string> split(const std::string& s) {
    std::stringstream ss(s);
    std::vector<std::string> result;
    std::string word;

    while (ss >> word) {
        result.push_back(word);
    }
    return result;
}

void parseCommand(std::string command){

  std::string name = command.substr(0, command.find(':'));

  std::vector<std::string> func_args = split(command.substr(command.find(':') + 1, command.length() - (command.find(':') + 1)));

  for(const auto& cmp : components){
    if(cmp->name == name){
      if(func_args[0] == "set" && func_args.size() > 1){
        cmp->set(std::vector<std::string>(func_args.begin()+1, func_args.end()));
      }else{
        cmp->status();
      }
    }
  }
}

void setup() {
  //components.push_back(std::make_unique<timerModule>(timer));
  components.push_back(std::make_unique<numberGameModule>(numberGame));
  //timer.set({"500"});
  LCDDisplay.LCDSetup();
}

void loop() {

  parseCommand(serialController.serialRecieve());
  numberGame.updateDisplay();
  //timer.updateDisplay();
  LCDDisplay.LCDLoop();
}
