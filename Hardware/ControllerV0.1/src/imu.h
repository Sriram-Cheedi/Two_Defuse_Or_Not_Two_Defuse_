#pragma once
#include "Component.h"
#include "SerialController.h"
#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>

class Imu : public Component {

private:


    SerialController serialController;

    Adafruit_MPU6050 mpu;

    bool enabled = true;

    bool motionActive = false;
    unsigned long lastMotionTime = 0;
    const unsigned long motionTimout = 200;

    float accelX;
    float accelY;
    float accelZ;
    
    float gyroX;
    float gyroY;
    float gyroZ;

public:

    Imu(std::string name, SerialController serialController);

    void update();

    void enable() override;
    void disable() override;
    void set(std::vector<std::string> args) override;
    void status() override;

};
