require 'albacore'
require 'yaml'

buildName = "Booksleeve"
buildNumber = (ENV['buildnumber'] || "0.0") + ".*"
fullBuildNumber = ENV['fullBuildnumber'] || "0.0"
solutionFile = "./BookSleeve_NoAsync.sln"

desc "Build the Configuration assembly"
task :default => [:buildRelease]

desc "Displays a list of tasks"
task :help do

  taskHash = Hash[*(`rake.bat -T`.split(/\n/).collect { |l| l.match(/rake (\S+)\s+\#\s(.+)/).to_a }.collect { |l| [l[1], l[2]] }).flatten]

  indent = "                          "

  puts "rake #{indent}#Runs the 'default' task"

  taskHash.each_pair do |key, value|
    if key.nil?
      next
    end
    puts "rake #{key}#{indent.slice(0, indent.length - key.length)}##{value}"
  end
end

msbuild :buildRelease do |msb|
	puts("Starting 	Build #{buildName}
					BuildNumber: #{buildNumber}")
	msb.properties = {:configuration => :Release, :TrackFileAccess => false}
	msb.targets :clean, :Build
	msb.solution = solutionFile
end

