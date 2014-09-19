// Adafruit Motor shield library
// copyright Adafruit Industries LLC, 2009
// this code is public domain, enjoy!

#include <AFMotor.h>

AF_DCMotor motor1(1); // front left
AF_DCMotor motor2(2); // rear left
AF_DCMotor motor3(3); // rear right
AF_DCMotor motor4(4); // front right
const int NM = 4;
AF_DCMotor motors[NM] = { motor1, motor2, motor3, motor4 };
int incomingByte;

void clearMotors() {
  uint8_t i;
  for (i = 0; i < NM; i++) {
    motors[i].setSpeed(200);
    motors[i].run(RELEASE);
  }  
}

void setup() {
  Serial.begin(9600);           // set up Serial library at 9600 bps

  clearMotors();
}

void loop() {
  uint8_t i;
  if (Serial.available() > 0) {
    // read the oldest byte in the serial buffer:
    incomingByte = Serial.read();
    if (incomingByte == 'W') {
      Serial.print("forward\n");
      for (i = 0; i < NM; i++) {
        motors[i].run(FORWARD);
      }
    } else if (incomingByte == 'S') {
      Serial.print("backward\n");
      for (i = 0; i < NM; i++) {
        motors[i].run(BACKWARD);
      }
    } else if (incomingByte == 'A') {
      Serial.print("left turn\n");
      motor3.run(FORWARD);
      motor4.run(FORWARD);
    } else if (incomingByte == 'D') {
      Serial.print("right turn\n");
      motor1.run(FORWARD);
      motor2.run(FORWARD);
    } else if (incomingByte == 'Q') {
      Serial.print("stop\n");
      for (i = 0; i < NM; i++) {
        motors[i].run(RELEASE);
      }
    }
  }
}
