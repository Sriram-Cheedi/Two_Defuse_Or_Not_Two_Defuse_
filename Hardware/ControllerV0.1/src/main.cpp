#include <Arduino.h>
#include <vector>
#include "TimerModule.h"
#include <sstream>
#include "Component.h"
#include "SerialController.h"
#include "Imu.h"
#include "Button.h"
#include <TeensyThreads.h>
#include "SimonSaysModule.h"

SerialController serialController = SerialController();

TimerModule timer = TimerModule("TMR", 29, 27, 28, serialController);
Imu gyro = Imu("IMU", serialController);

Button btn1 = Button("BTN1", 38, serialController);
Button btn2 = Button("BTN2", 39, serialController);
Button btn3 = Button("BTN3", 41, serialController);

SimonSaysModule ssm = SimonSaysModule("SSM", {25, 24, 10, 9}, 6,  serialController);

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
  components.push_back(&timer);
  components.push_back(&gyro);  
  components.push_back(&ssm);

}

void loop() {

  delay(1);

  parseCommand(serialController.serialRecieve());

  btn1.update();
  btn2.update();
  btn3.update(); 

  timer.updateDisplay();
  gyro.update();
  ssm.update();
}
