#include <Arduino.h>
#include <vector>
#include "TimerModule.h"
#include <sstream>
#include "Component.h"
#include "SerialController.h"
#include "Imu.h"
#include "Button.h"
#include <TeensyThreads.h>

SerialController serialController = SerialController();

TimerModule timer = TimerModule("TMR", 27, 25, 26, serialController);
Imu gyro = Imu("IMU", serialController);

void buttonPress(){
  Serial.printf("A BUTTON IS PRESSED\n");
}

Button testButton = Button("TestBtn", 38, serialController, buttonPress);

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

volatile int i = 600;

void countDown(){
  while(i >= 0){
    timer.set({std::to_string(i)});
    threads.delay(1000);
    i--;
  }
}




void setup() {
  components.push_back(&timer);
  components.push_back(&gyro);  
  components.push_back(&testButton);


  threads.addThread(countDown, 1);

}

void loop() {

  parseCommand(serialController.serialRecieve());


  timer.updateDisplay();
  gyro.update();
  testButton.update();
}
