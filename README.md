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
油膜圧力の支配方程式には下記の「三次元ﾚｲﾉﾙｽﾞ方程式」があります。
<br>
<br>
<img src="https://user-images.githubusercontent.com/52177886/68773045-398e0780-066e-11ea-8863-1fae1f27507b.jpg" height="50px" alt="三次元レイノルズ方程式">
<br>
<br>
ここで、P、h、ηは油膜圧力、ｼｰﾙﾘｯﾌﾟとｼｬﾌﾄの隙間距離、ｵｲﾙ粘度。U、V、WはそれぞれX、y、z方向の速度とします。
<br>
下図のような系を考えます。
<br>
<br>
<img src="https://user-images.githubusercontent.com/52177886/68772400-16af2380-066d-11ea-9685-d4498cb13b94.jpg" width="500px" alt="座標系">
<br>
<br>
ここで、三次元レイノルズ方程式を簡単化するために幾つかの仮定をする。
<br>
　　(仮定1) ｼｰﾙﾘｯﾌﾟおよびｼｬﾌﾄを剛体とする。
<br>
  　(仮定2) ｼｰﾙﾘｯﾌﾟとｼｬﾌﾄ間の隙間距離が変化せず、静止したｼｰﾙﾘｯﾌﾟに対してｼｬﾌﾄ表面が速度U1で移動する。
<br>
仮定1より面の伸縮を表す三次元レイノルズ方程式の右辺第2項および第4項が0となる。また、
<br>
仮定2より右辺第3項および第5項が0、右辺第1項はU2が0となる。
<br><br>
<img src="https://user-images.githubusercontent.com/52177886/69241289-d65a2300-0be1-11ea-8a42-0736b65340bf.jpg" height="50px" alt="簡略化条件">
<br><br>
以上の仮定から、三次元レイノルズ方程式は次のようになる。
<br><br>
<img src="https://user-images.githubusercontent.com/52177886/69241318-e7a32f80-0be1-11ea-93c9-52c0a0592298.jpg" height="50px" alt="簡略化した三次元ﾚｲﾙｽﾞ方程式">
<br><br>