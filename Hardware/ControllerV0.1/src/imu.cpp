#include "Imu.h"



Imu::Imu(std::string name, SerialController serialController)
: Component(name)
{

    mpu.begin();

    //setupt motion detection
    mpu.setHighPassFilter(MPU6050_HIGHPASS_0_63_HZ);
    mpu.setMotionDetectionThreshold(1);
    mpu.setMotionDetectionDuration(2);
    mpu.setInterruptPinLatch(true);	// Keep it latched.  Will turn off when reinitialized.
    mpu.setInterruptPinPolarity(true);
    mpu.setMotionInterrupt(true);

    delay(100);
}

void Imu::update(){

    if(mpu.getMotionInterruptStatus()) {
        motionActive = true;
        lastMotionTime = millis();

    }

    sensors_event_t a, g, temp;
    mpu.getEvent(&a, &g, &temp);

    float maxGyro = max(max(fabs(g.gyro.x),
                            fabs(g.gyro.y)),
                            fabs(g.gyro.z));

    if (motionActive && maxGyro > 0.2){
        /* Get new sensor events with the readings */

        lastMotionTime = millis();
        
        gyroX = g.gyro.x;
        gyroY = g.gyro.y;
        gyroZ = g.gyro.z;

        accelX = a.acceleration.x;
        accelY = a.acceleration.y;
        accelZ = a.acceleration.z;

        status();

        if (millis() - lastMotionTime > motionTimout){
            motionActive = false;
        }
    }
}

void Imu::set(std::vector<std::string> args){
    return;
}

auto to_string_2dp = [](float v) {
    char buf[16];
    snprintf(buf, sizeof(buf), "%.2f", v); 
    return std::string(buf);
};

void Imu::status(){

    serialController.serialWrite(name, {to_string_2dp(gyroX),
                                        to_string_2dp(gyroY),
                                        to_string_2dp(gyroZ),
                                        to_string_2dp(accelX),
                                        to_string_2dp(accelY),
                                        to_string_2dp(accelZ)
                                        });

}