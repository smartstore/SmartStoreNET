pngquant --quality=90-99 --output %2 %1 --force
if not exist %2 copy %1 %2 /y

truepng %2 /o4 