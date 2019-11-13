# Fluid_lubrication
三次元レイノルズ方程式をComputeShaderで計算するプロジェクト

# Overview
ｵｲﾙｼｰﾙの大気側に十分なｵｲﾙが存在している場合に、ｼｬﾌﾄの回転に伴いｵｲﾙ側にﾎﾟﾝﾋﾟﾝｸﾞ作用が発生します。
<br>
ｵｲﾙｼｰﾙの密封性能の向上ために、ｼｰﾙﾘｯﾌﾟ大気面に「ﾘﾌﾞ」と呼ばれる突起を付与して
<br>
ﾎﾟﾝﾋﾟﾝｸﾞ作用を補強する場合があります。
<br>
ﾘﾌﾞ周辺はｸｻﾋﾞ効果によりｵｲﾙ油膜に圧力が発生し、この油膜圧の分布がﾎﾟﾝﾋﾟﾝｸﾞ作用の要因の一つと考えられます。
<br>
油膜圧力の支配方程式には下記の「三次元ﾚｲﾉﾙｽﾞ方程式」があり、
<br>
<img src="https://user-images.githubusercontent.com/52177886/68769720-28da9300-0668-11ea-99c6-a4eca60eff3b.jpg" height="50px" alt="三次元レイノルズ方程式">

<br>
<img src="https://user-images.githubusercontent.com/52177886/68772400-16af2380-066d-11ea-9685-d4498cb13b94.jpg" width="500px" alt="座標系">
<br>
