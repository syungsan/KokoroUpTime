# -*- coding: sjis -*-

# require "win32ole"
# wsh = WIN32OLE.new("Wscript.Shell")

# ���̋N���X�N���v�g�̃p�X�����ƂȂ�
BASE_ABSOLUTE_PATH = File.dirname(File.expand_path('.', __FILE__))

# cmd = "cmd /c #{BASE_ABSOLUTE_PATH}/TabTip.bat"
# wsh.Run(cmd, 0, false)

Dir.chdir("#{BASE_ABSOLUTE_PATH}/Programs") do

  # cd ��ł̏���������
  system("#{BASE_ABSOLUTE_PATH}/Programs/KokoroUpTime.exe")
end

exit
