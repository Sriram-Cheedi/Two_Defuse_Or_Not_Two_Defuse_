#include <Arduino.h>
#include <vector>
#include <bits/stdc++.h>
#include "timerModule.h"
#include "component.h"
#include "serialController.h"
#include "imu.h"

SerialController serialController = SerialController();

timerModule timer = timerModule("TMR", 27, 25, 26, serialController);
imu gyro = imu("IMU", serialController);

std::vector<Component*> components;

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

  if(command == "") return;

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
  components.reserve(50);

  components.push_back(&timer);
  components.push_back(&gyro);  

  timer.set({"0"});

}

void loop() {

  parseCommand(serialController.serialRecieve());

  timer.updateDisplay();
  gyro.update();
}
