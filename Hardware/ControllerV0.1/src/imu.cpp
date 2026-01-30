#include "imu.h"



imu::imu(std::string name, SerialController serialController)
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



void imu::update(){

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

        status();

        if (millis() - lastMotionTime > motionTimout){
            motionActive = false;
        }
    }
}

void imu::set(std::vector<std::string> args){
    return;
}

void imu::status(){

    serialController.serialWrite(name, {std::to_string(gyroX), std::to_string(gyroY), std::to_string(gyroZ)});

}