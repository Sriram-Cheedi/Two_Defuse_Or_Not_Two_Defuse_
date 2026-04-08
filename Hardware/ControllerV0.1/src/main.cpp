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
#include "numberGameModule.h"
#include "fishingModule.h"
#include "mazeModule.h"
#include "Buzzer.h"

SerialController serialController = SerialController();


std::vector<Component*> components; 

TimerModule timer = TimerModule("TMR", 30, 32, 31, serialController);
Imu gyro = Imu("IMU", serialController);

LCDModule LCD = LCDModule("LCD", serialController);
MazeModule maze = MazeModule("MAZE", serialController, LCD);


Buzzer buzzer = Buzzer("BUZZER", 28, serialController, 50);

int flickerThread = -1;

void flickerOn(){
  for(int i = 0; i < 4; i++){
    for(const auto& cmp : components){
      if(cmp->name != "IMU" &&
          cmp->name != "PWR") cmp->enable();
    }
    uint32_t waitUntil = millis() + 50;
    while (millis() < waitUntil) threads.yield();

    for(const auto& cmp : components){
      if(cmp->name != "IMU" &&
          cmp->name != "PWR") cmp->disable();
    }
    waitUntil = millis() + 100;
    while (millis() < waitUntil) threads.yield();
  }

  uint32_t waitUntil = millis() + 100;
  while (millis() < waitUntil) threads.yield();

  for(const auto& cmp : components){
    cmp->enable();
  }
}


void powerToggle(std::string btnName, int btnState) {
  if(btnState == 0){

    if(flickerThread != -1){
      threads.kill(flickerThread);
      flickerThread = -1;
    }

    for(const auto& cmp : components){
      if(cmp->name != "IMU" &&
         cmp->name != "PWR") {
          cmp->disable();
      }else{
        cmp->enable();
      }
    }
  }else{
    flickerThread = threads.addThread(flickerOn);
  }

  serialController.serialWrite("PWR", std::to_string(btnState));
}

void siren(std::string btnName, int btnState){
  if(btnState == 0){
    buzzer.sirenOff();
  }else{
    buzzer.sirenOn();
  }
}

void startNotification(std::string btnName, int btnState){
  if(btnState == 1) buzzer.notification();
}


Button btn1 = Button("BTN1", 9, serialController);
Button btn2 = Button("BTN2", 10, serialController);
Button btn3 = Button("BTN3", 11, serialController);
Button btn4 = Button("BTN4", 12, serialController);

Button wire1 = Button("WR1", 23, serialController);
Button wire2 = Button("WR2", 22, serialController);
Button wire3 = Button("WR3", 21, serialController);
Button wire4 = Button("WR4", 20, serialController);

Button pwr = Button("PWR", 7, serialController, powerToggle);

Button sw1 = Button("SW1", 6, serialController);
Button sw2 = Button("SW2", 5, serialController);
Button sw3 = Button("SW3", 4, serialController);


numberGameModule ngm = numberGameModule("NGM",33,35,34,{36,37,38,39,40}, serialController);
fishingModule fgm = fishingModule("FGM",16,14,15,17,serialController);
SimonSaysModule ssm = SimonSaysModule("SSM", {24, 25, 26, 27}, 0,  serialController);




void powerGame(){

  if(timer.getEnabled()){
    for(const auto& cmp : components){
      if(cmp->name != "IMU" &&
          cmp->name != "PWR" &&
          cmp->name != "LCD" ) cmp->scramble();
    }

    LCD.set({"Unknown Error!!!", "Power Cycle Req."});
  }

}


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

  if(name == "PWR" && func_args[0] == "set"){
    if(std::stoi(func_args[1]) == 3){
      powerGame();
      return;
    }else if(std::stoi(func_args[1]) == 4){
      for(const auto& cmp : components){
        cmp->disable();
      }
      return;
    }else if(std::stoi(func_args[1]) == 5){
      pwr.enable();
      pwr.update();
      powerToggle("PWR", pwr.lastState);

      LCD.set({"\"\"", "\"\""});

      return;
    }
    powerToggle(name, std::stoi(func_args[1]));
  }


  if(name == "GLB" && func_args[0] == "status"){
    for(const auto& cmp : components){
      cmp->update();
      cmp->status();
    }
  }

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
  components.push_back(&buzzer);
  components.push_back(&timer);
  components.push_back(&gyro);  
  components.push_back(&ssm);
  components.push_back(&ngm);
  components.push_back(&fgm);
  components.push_back(&btn1);
  components.push_back(&btn2);
  components.push_back(&btn3);
  components.push_back(&btn4);
  components.push_back(&pwr);
  components.push_back(&sw1);
  components.push_back(&sw2);
  components.push_back(&sw3);
  components.push_back(&LCD);
  components.push_back(&maze);
  components.push_back(&wire1);
  components.push_back(&wire2);
  components.push_back(&wire3);
  components.push_back(&wire4);

  LCD.LCDSetup();

  digitalWrite(8, HIGH); //Power pin for pwr switch#
  digitalWrite(13, HIGH); //Power pin for FGM

}

void loop() {

  delay(1);

  parseCommand(serialController.serialRecieve());

  for(const auto& cmp : components){
    cmp->update();
  }
}
