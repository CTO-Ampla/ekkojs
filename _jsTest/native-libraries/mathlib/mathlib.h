#ifndef MATHLIB_H
#define MATHLIB_H

#ifdef _WIN32
    #ifdef MATHLIB_EXPORTS
        #define MATHLIB_API __declspec(dllexport)
    #else
        #define MATHLIB_API __declspec(dllimport)
    #endif
#else
    #define MATHLIB_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

// Basic math operations
MATHLIB_API int add(int a, int b);
MATHLIB_API int subtract(int a, int b);
MATHLIB_API double multiply_double(double a, double b);
MATHLIB_API double divide_double(double a, double b);

// String operations
MATHLIB_API const char* get_version();
MATHLIB_API int string_length(const char* str);
MATHLIB_API void reverse_string(char* str);

// Struct example
typedef struct {
    double x;
    double y;
} Point;

MATHLIB_API double distance(Point* p1, Point* p2);
MATHLIB_API void translate_point(Point* p, double dx, double dy);

// Callback example
typedef int (*math_callback)(int a, int b);
MATHLIB_API int apply_operation(int a, int b, math_callback callback);

// Array operations
MATHLIB_API int sum_array(const int* arr, int size);
MATHLIB_API void double_array(int* arr, int size);

#ifdef __cplusplus
}
#endif

#endif // MATHLIB_H