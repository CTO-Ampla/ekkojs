#include "mathlib.h"
#include <math.h>
#include <string.h>
#include <stdlib.h>

// Basic math operations
int add(int a, int b) {
    return a + b;
}

int subtract(int a, int b) {
    return a - b;
}

double multiply_double(double a, double b) {
    return a * b;
}

double divide_double(double a, double b) {
    if (b == 0.0) {
        return 0.0; // Simple error handling
    }
    return a / b;
}

// String operations
const char* get_version() {
    return "MathLib v1.0.0";
}

int string_length(const char* str) {
    if (str == NULL) {
        return 0;
    }
    return (int)strlen(str);
}

void reverse_string(char* str) {
    if (str == NULL || *str == '\0') {
        return;
    }
    
    int len = strlen(str);
    for (int i = 0; i < len / 2; i++) {
        char temp = str[i];
        str[i] = str[len - 1 - i];
        str[len - 1 - i] = temp;
    }
}

// Struct operations
double distance(Point* p1, Point* p2) {
    if (p1 == NULL || p2 == NULL) {
        return 0.0;
    }
    
    double dx = p2->x - p1->x;
    double dy = p2->y - p1->y;
    return sqrt(dx * dx + dy * dy);
}

void translate_point(Point* p, double dx, double dy) {
    if (p == NULL) {
        return;
    }
    
    p->x += dx;
    p->y += dy;
}

// Callback example
int apply_operation(int a, int b, math_callback callback) {
    if (callback == NULL) {
        return 0;
    }
    return callback(a, b);
}

// Array operations
int sum_array(const int* arr, int size) {
    if (arr == NULL || size <= 0) {
        return 0;
    }
    
    int sum = 0;
    for (int i = 0; i < size; i++) {
        sum += arr[i];
    }
    return sum;
}

void double_array(int* arr, int size) {
    if (arr == NULL || size <= 0) {
        return;
    }
    
    for (int i = 0; i < size; i++) {
        arr[i] *= 2;
    }
}