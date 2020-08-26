# -*- coding: sjis -*-

# この起動スクリプトのパスが基底となる
BASE_ABSOLUTE_PATH = File.dirname(File.expand_path('.', __FILE__))

Dir.chdir("#{BASE_ABSOLUTE_PATH}/KokoroUpTime") do

  # cd 先での処理を書く
  system("#{BASE_ABSOLUTE_PATH}/KokoroUpTime/KokoroUpTime.exe")
end

exit
