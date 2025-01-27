//[astronomy](../../../index.md)/[io.github.cosinekitty.astronomy](../index.md)/[RotationMatrix](index.md)

# RotationMatrix

class [RotationMatrix](index.md)(rot: [Array](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-array/index.html)&lt;[DoubleArray](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double-array/index.html)&gt;)

A rotation matrix that can be used to transform one coordinate system to another.

## Constructors

| | |
|---|---|
| [RotationMatrix](-rotation-matrix.md)<br>fun [RotationMatrix](-rotation-matrix.md)(a00: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a01: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a02: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a10: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a11: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a12: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a20: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a21: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html), a22: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)) |
| [RotationMatrix](-rotation-matrix.md)<br>fun [RotationMatrix](-rotation-matrix.md)(rot: [Array](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-array/index.html)&lt;[DoubleArray](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double-array/index.html)&gt;) |

## Types

| Name | Summary |
|---|---|
| [Companion](-companion/index.md)<br>object [Companion](-companion/index.md) |

## Functions

| Name | Summary |
|---|---|
| [combine](combine.md)<br>infix fun [combine](combine.md)(other: [RotationMatrix](index.md)): [RotationMatrix](index.md)<br>Creates a rotation based on applying one rotation followed by another. |
| [inverse](inverse.md)<br>fun [inverse](inverse.md)(): [RotationMatrix](index.md)<br>Calculates the inverse of a rotation matrix. |
| [pivot](pivot.md)<br>fun [pivot](pivot.md)(axis: [Int](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-int/index.html), angle: [Double](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double/index.html)): [RotationMatrix](index.md)<br>Re-orients the rotation matrix by pivoting it by an angle around one of its axes. |
| [rotate](rotate.md)<br>fun [rotate](rotate.md)(state: [StateVector](../-state-vector/index.md)): [StateVector](../-state-vector/index.md)<br>Applies a rotation to a state vector, yielding a rotated state vector.<br>[jvm]<br>fun [rotate](rotate.md)(vec: [Vector](../-vector/index.md)): [Vector](../-vector/index.md)<br>Applies a rotation to a vector, yielding a rotated vector. |

## Properties

| Name | Summary |
|---|---|
| [rot](rot.md)<br>val [rot](rot.md): [Array](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-array/index.html)&lt;[DoubleArray](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin-stdlib/kotlin/-double-array/index.html)&gt;<br>A 3x3 array of numbers to initialize the rotation matrix. |
